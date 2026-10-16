# Unity MCP smoke test: create and color a cube

Use the current Unity Editor and the active test scene. Do not modify unrelated objects or settings.

Goal:
- Ensure exactly one GameObject named `AgentSmokeTestCube` exists.
- It is a Unity Cube primitive.
- Position `(0, 0, 0)`, rotation `(0, 0, 0)`, scale `(1, 1, 1)`.
- Its visible renderer color is red.
- Save the scene only if the user has explicitly asked for persistence or the test scene is disposable.

Required process:
1. Read the current project info and active scene. Determine render pipeline.
2. Search for `AgentSmokeTestCube`. Reuse it if present; do not create duplicates.
3. Create it if absent and verify the object by reading the scene/hierarchy.
4. Inspect the actual schema of the available material/renderer tools.
5. Prefer the supported direct renderer-color action if available. In current CoplayDev Unity MCP documentation, `manage_material` may expose `action="set_renderer_color"` with `color=[1,0,0,1]` and `mode="create_unique"`. Verify that this is the active schema before using it.
6. Do not call `assign_material_to_renderer` with a path unless the material exists. If a material must be created, choose a shader compatible with the detected render pipeline.
7. Read back the renderer/material state and verify red color.
8. If a step fails, classify the failure and change the approach. Do not repeat the same invalid call unchanged.
9. Report actual tool calls performed, verification evidence, and anything not verified.

Do not report success based solely on a tool invocation response.
