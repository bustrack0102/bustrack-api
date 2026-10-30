---
name: testing-pact
description: Captura sin Pact real (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# testing-pact MVP (lite)

## Incluye
- `FixtureCaptureTests WAF Staging + CONTRACT_CAPTURE=1 + IsConventional legacy + RowVersion AQ==`.

## Excluye
- PactNet + provider proceso real + puerto libre.

## Verify
- `CONTRACT_CAPTURE=1` regenera, sin var solo valida + `xunit.runner.json` serie.
