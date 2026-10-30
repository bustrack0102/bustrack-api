"""Puerta incremental nv-03: cobertura del delta vs main (advisory por defecto).

Delta = archivos *.cs bajo API_SDD/ cambiados (git merge-base <ref> HEAD,
diff commiteado + archivos nuevos sin seguimiento), menos higiene:
  - **/Migrations/* (generado EF)
  - **/Program.cs y cableado de arranque
  - **/Dtos/* (modelos anemicos, solo get/set)

La union de lineas entre reportes evita el doble conteo entre suites.
En modo --advisory informa y sale 0 aunque no alcance --min.
El unico bloqueo real sigue siendo check_coverage.py --min 45 (constitucion 7).
Uso: python scripts/check_delta_coverage.py --min 80 --advisory
"""
import argparse
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

HYGIENE = ("Migrations/", "Program.cs", "Dtos/", ".Designer.cs")
SRC_ROOT = "API_SDD"


def run_git(args: list[str]) -> str:
    out = subprocess.run(
        ["git", *args], capture_output=True, text=True, check=False
    )
    return out.stdout if out.returncode == 0 else ""


def delta_files(ref: str) -> list[str]:
    base = run_git(["merge-base", ref, "HEAD"]).strip()
    if not base:
        print(f"WARN: sin base contra {ref}, se asume delta vacio.")
        return []
    tracked = run_git(
        ["diff", "--name-only", base, "HEAD", "--", f"{SRC_ROOT}/**/*.cs"]
    ).splitlines()
    # Arbol local: incluye modificados sin commitear (en CI el arbol esta
    # limpio y no aporta nada).
    worktree = run_git(
        ["diff", "--name-only", "HEAD", "--", f"{SRC_ROOT}/**/*.cs"]
    ).splitlines()
    untracked = [
        line[3:].strip().strip('"')
        for line in run_git(
            ["status", "--porcelain", "--untracked-files=all", "--", SRC_ROOT]
        ).splitlines()
        if line.startswith("??")
    ]
    seen: list[str] = []
    for raw in [*tracked, *worktree, *untracked]:
        path = raw.replace("\\", "/").strip()
        if not path.endswith(".cs"):
            continue
        if any(h in path for h in HYGIENE):
            continue
        if path not in seen:
            seen.append(path)
    return seen


def file_lines(report: Path, suffix: str) -> dict[int, bool]:
    """Linea -> cubierta (union entre reportes con OR)."""
    covered: dict[int, bool] = {}
    try:
        root = ET.parse(report).getroot()
    except ET.ParseError:
        return covered
    for cls in root.iter("class"):
        # Coverlet emite rutas relativas al proyecto (Validators/X.cs) o
        # absolutas; el match es por sufijo en ambas direcciones.
        name = (cls.get("filename") or "").replace("\\", "/")
        if not (name.endswith(suffix) or suffix.endswith(name)):
            continue
        for line in cls.iter("line"):
            try:
                number = int(line.get("number", "0"))
                hits = int(line.get("hits", "0"))
            except ValueError:
                continue
            if number <= 0:
                continue
            covered[number] = covered.get(number, False) or hits > 0
    return covered


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--min", type=float, default=80.0)
    ap.add_argument("--advisory", action="store_true")
    ap.add_argument("--ref", default="main")
    ap.add_argument("--results", default="TestResults")
    args = ap.parse_args()

    files = delta_files(args.ref)
    if not files:
        print("Delta vacio: sin archivos medibles. OK.")
        return 0

    reports = sorted(Path(args.results).rglob("coverage.cobertura.xml"))
    if not reports:
        print("WARN: sin reportes de cobertura, no se puede medir el delta.")
        return 0 if args.advisory else 1

    total_valid = 0
    total_covered = 0
    for path in files:
        suffix = path[path.find(SRC_ROOT):] if SRC_ROOT in path else path
        merged: dict[int, bool] = {}
        for report in reports:
            for number, hit in file_lines(report, suffix).items():
                merged[number] = merged.get(number, False) or hit
        valid = len(merged)
        covered = sum(1 for hit in merged.values() if hit)
        total_valid += valid
        total_covered += covered
        tag = "sin lineas medibles" if valid == 0 else f"{covered}/{valid}"
        print(f"  {path}: {tag}")

    pct = round(100.0 * total_covered / total_valid, 2) if total_valid > 0 else 100.0
    print(f"Delta: {total_covered}/{total_valid} = {pct}% (min {args.min}%)")
    if pct >= args.min:
        print("OK: delta alcanza el umbral.")
        return 0
    if args.advisory:
        print("WARN advisory: delta bajo el umbral, no bloquea (bloqueo real: 45% global).")
        return 0
    print(f"FAIL: delta {pct}% < {args.min}%")
    return 1


if __name__ == "__main__":
    sys.exit(main())
