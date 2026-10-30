"""Gate cobertura >=45%. Constitución §7."""
import argparse
import re
import sys
from pathlib import Path


def parse_coverage(path: Path) -> float:
    text = path.read_text(encoding="utf-8", errors="ignore")
    m = re.search(r"Line coverage:\s*([0-9]+(?:\.[0-9]+)?)\s*%", text)
    if m:
        return float(m.group(1))
    m = re.search(r"([0-9]+(?:\.[0-9]+)?)\s*%", text)
    if m:
        return float(m.group(1))
    return 0.0


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--min", type=float, default=45.0)
    ap.add_argument("--report", default="coverage.txt")
    args = ap.parse_args()
    report = Path(args.report)
    if not report.exists():
        print(f"WARN: {report} no existe, se asume 0%. Generar con coverlet primero.")
        print(f"FAIL: cobertura 0.0% < {args.min}%")
        return 1
    pct = parse_coverage(report)
    print(f"Cobertura: {pct}% (min {args.min}%)")
    return 0 if pct >= args.min else 1


if __name__ == "__main__":
    sys.exit(main())
