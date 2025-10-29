# Neon Pedestal UCC (URP) - v3
Incluye dos formas de generar el pedestal con anillo neón y logo UCC transparente.

## Opción A: Menú Tools (recomendado)
1. Copia `Assets/NeonPedestal` dentro de tu proyecto (URP).
2. En Unity: **Tools → Neon Pedestal (UCC) → Create Prefab**.
3. Aparecerá el prefab en `Assets/NeonPedestal/Prefabs/NeonPedestal_UCC.prefab`.

## Opción B: Componente (ExecuteAlways)
1. Crea un Empty GameObject en tu escena.
2. Añade el componente **NeonPedestalSpawner**.
3. En el Inspector, pulsa **Rebuild Now** (marca `rebuildNow`).
4. Se generará toda la jerarquía (base + anillo torus + logo + luz).

## Requisitos
- Proyecto con **Universal Render Pipeline (URP)**.
- Agrega un **Global Volume** con **Bloom** para ver el halo neón.

## Notas
- El logo se encuentra en `Assets/NeonPedestal/Textures/UC_Logo.png` (transparente).
- Puedes sustituirlo por otro (mismo nombre) para que se reasigne automáticamente.
