---
name: 09-cicd-ops
description: CI PR sin CD (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 09-cicd-ops MVP (lite)

## Incluye
- `PR 1 slice + checklist + 1 revisor + publish manual + tag`.

## Excluye
- CloudFormation, ALB/EC2/RDS/SQS, schedule-deploy/destroy.

## Verify
- `CHECKLIST_PR.md` + tag `vX.Y.Z-crudN` + `dotnet publish -c Release -o ./publish`.
