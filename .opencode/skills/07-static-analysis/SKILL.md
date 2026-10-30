---
name: 07-static-analysis
description: Semgrep+Sonar sin gate SonarCloud (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 07-static-analysis MVP (lite)

## Incluye
- `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error + SonarAnalyzer + S1541≤10/S3776≤15`.

## Excluye
- Quality Gate SonarCloud obligatorio, new coverage 80%.

## Verify
- Semgrep 0 nuevos + `.editorconfig` CA3000+ error.
