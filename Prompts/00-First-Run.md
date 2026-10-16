# First-run diagnostic prompt

Work in diagnostic mode. Do not modify files, scenes, assets, packages, project settings, or configuration.

1. Identify the Unity MCP server and enumerate the tools/groups actually exposed in this session.
2. Read the Unity project info resource if available. Report Unity version, project root/name, render pipeline, active input handler, and relevant installed packages. If a field is unavailable, say so.
3. Read the active scene and summarize its current hierarchy without changing it.
4. Check the project root for existing design, architecture, coding-rule, and test documents. Read the relevant ones; do not invent missing project facts.
5. Report the exact model/provider information visible to you only if exposed; do not guess.
6. Based on actual evidence, identify the three most likely reliability risks and give a minimal diagnostic plan.

For every factual claim, distinguish observed evidence from inference. Do not make any changes during this task.
