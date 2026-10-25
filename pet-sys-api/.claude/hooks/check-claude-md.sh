#!/usr/bin/env bash
# Stop hook: pide revisar CLAUDE.md si cambió código desde el último commit de CLAUDE.md.
cd "${CLAUDE_PROJECT_DIR:-.}" || { echo '{}'; exit 0; }

# Si ya bloqueó una vez en esta respuesta, deja terminar a Claude; si no, entra en loop.
if grep -q '"stop_hook_active" *: *true'; then echo '{}'; exit 0; fi

if [ -n "$(git status --porcelain -- CLAUDE.md 2>/dev/null)" ]; then echo '{}'; exit 0; fi

code=(-- '*.cs' '*.csproj' '*.sln' ':!*/bin/*' ':!*/obj/*')

# Las líneas en blanco y los comentarios // no cambian lo que documenta CLAUDE.md.
real_lines() { grep -E '^[+-]' | grep -vE '^(\+\+\+|---) ' | grep -vE '^[+-][[:space:]]*(//.*)?$'; }

last=$(git log -1 --format=%H -- CLAUDE.md 2>/dev/null)
changed=""
[ -n "$last" ] && changed=$(git diff "$last" HEAD "${code[@]}" 2>/dev/null | real_lines)
[ -z "$changed" ] && changed=$(git diff HEAD "${code[@]}" 2>/dev/null | real_lines)
[ -z "$changed" ] && changed=$(git ls-files --others --exclude-standard "${code[@]}" 2>/dev/null)

if [ -n "$changed" ]; then
  echo '{"decision":"block","reason":"Hay cambios de codigo (.cs/.csproj/.sln), commiteados o no, que no se reflejaron en CLAUDE.md. Revisa si CLAUDE.md necesita actualizarse (nuevas entidades, capas, comandos o decisiones de arquitectura) antes de terminar."}'
else
  echo '{}'
fi
