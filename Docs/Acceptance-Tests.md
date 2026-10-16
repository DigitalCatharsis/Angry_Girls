# Acceptance Tests: Cline + Ollama + Unity MCP

Запускай тесты в отдельной тестовой сцене или в копии проекта. Не тестируй на сцене, где есть несохранённые пользовательские изменения.

## Test 0 — Model/tool contract
- Cline показывает правильную модель `qwen3-coder:30b`.
- MCP for Unity подключён.
- Агент может перечислить доступные Unity tools/groups без выдумывания.
- Агент отличает инструмент Cline для файлов от инструментов Unity MCP.
Pass: реальные названия инструментов совпадают с доступным списком; агент не сочиняет недоступные функции.

## Test 1 — Read-only discovery
Prompt: "Не меняй проект. Получи сведения о Unity project, активной сцене, render pipeline и доступных группах MCP. Перечисли источники каждого факта."
Pass: информация получена инструментами; неизвестные поля явно отмечены.

## Test 2 — Create cube
Prompt: "В текущей тестовой сцене создай один 3D Cube с именем AgentSmokeTestCube в позиции (0,0,0), scale (1,1,1). Не создавай материал. Прочитай и проверь состояние объекта после создания."
Pass: объект существует ровно один раз и значения Transform подтверждены read-back.

## Test 3 — Red color (core regression)
Prompt: "Покрась AgentSmokeTestCube в красный. Сначала изучи фактическую схему доступных material tools. Не назначай материал по пути, пока не подтвердил его существование. Предпочти прямое поддерживаемое действие изменения renderer color; если его нет, создай подходящий материал для render pipeline, проверь asset и назначь его. Затем прочитай результат и подтверди цвет."
Pass:
- цвет действительно красный;
- нет ошибки missing material;
- существование ресурса подтверждено, если использовался persistent asset;
- агент объясняет проверку, а не только заявляет успех.

## Test 4 — Recover from intentional invalid path
Prompt: "Попробуй получить материал по заведомо несуществующему пути Assets/Materials/__MISSING_TEST__.mat. Не создавай и не меняй никаких assets. Проанализируй ошибку и сообщи точную причину."
Pass: агент не сообщает ложный успех и не зацикливается на повторении идентичного вызова.

## Test 5 — Script change and compile
Only after the previous tests pass, ask the agent to make a tiny isolated C# change in a disposable branch/copy.
Pass: diff reviewed; Unity refresh/compile requested where supported; console checked; no claim of successful compilation without evidence.

## Scoring
For each test mark:
- 0 = failed or fabricated result
- 1 = partially correct, missing independent verification
- 2 = correct tool use and verified result

Interpretation:
- Model/tool discovery fails: investigate Cline/Ollama tool calling and exposed MCP schemas.
- Discovery passes, scene edit fails: investigate MCP tool schema, active instance, and server logs.
- Simple tests pass, large feature fails: improve task scope, project context, architecture map, and verification loop.
- Cloud model passes identical test but local model fails: evidence for model capability/tool-call quality difference, after confirming same tools and context.
