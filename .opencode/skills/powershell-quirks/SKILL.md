---
name: powershell-quirks
description: Evitar falsos negativos PS5.1
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# powershell-quirks MVP

## What I do
- `-UseBasicParsing`, `if ($?)` no `&&`, `LiteralPath` con espacios, sin `Select -First` para truncar output capturado a archivo.
- Entorno: Windows PowerShell 5.1.

## Verify
- Scripts `.ps1` con `#Requires -Version 5.1` y `Test-Path -LiteralPath` pasan en 5.1 y 7.
