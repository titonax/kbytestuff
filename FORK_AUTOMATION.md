# Automatización de titonax/kbytestuff

Este fork prepara dos variantes sin firma, con el mismo número de versión de upstream:

- `WandEnhancer-<version>-offline.exe`: excluye el código de consulta de actualizaciones.
- `WandEnhancer-<version>-update-notifications.exe`: incluye `EnableUpdateNotifications` y consulta las Releases de `titonax/kbytestuff`. Notifica versiones; no descarga ni instala actualizaciones.

Cada variante incluye además un ZIP con el ejecutable con su nombre original, archivos de ejecución, licencia, changelog y `build-info.json` (repositorio, commit, versión y variante), y un archivo SHA256. Se recomienda usar el ZIP para conservar juntos los archivos de ejecución.

## Estado del código recuperado

El antiguo `titonax/Wand-Enhancer` responde HTTP 451 por DMCA y no permite recuperar sus workflows personalizados, historial ni Actions. El chat de referencia solo contiene la solicitud de recuperación. No se ha podido verificar la configuración exacta del antiguo fork.

Sí se recuperó el workflow `build.yml` del historial disponible de upstream, anterior al commit `428b1addd3dfa5c6160d848ef188f27113e4d2a6` que lo eliminó. Se conservan Windows, Node 22, pnpm 10.17.0, compilación Release, las pruebas de `build.ps1`, la opción `EnableUpdateNotifications` y los artefactos sin firma. La matriz ahora genera y conserva ambas variantes por separado.

La versión actual de upstream, 2.2.1.0, no compila porque retiró estas fuentes:

- `WandEnhancer/Core/EnhancerConfig.cs`
- `WandEnhancer/Core/Patching/Shared/ElectronFuseWire.cs`
- `WandEnhancer/Core/Patching/Shared/DiskFusePatch.cs`

El propietario aportó sus dos ejecutables 2.1.1.0 y se reconstruyeron estas tres clases y dos recursos JavaScript ausentes. [RECOVERY.md](RECOVERY.md) recoge los hashes, herramientas y límites de la recuperación. El workflow conserva una comprobación de fuentes completas y no publica sin ambas variantes verificadas.

## Workflows

`Build offline and update executables` permite ejecución manual y comprueba los cambios de master y los PR. Compila y prueba cada variante de forma aislada, y conserva sus artefactos 30 días. También es reutilizable por el proceso de publicación.

`Update from upstream and publish` consulta cada día a las 07:23 UTC la última Release estable de `k1tbyte/Wand-Enhancer`. También se puede ejecutar manualmente desde master. Usa el commit exacto del tag, no el master mutable de upstream. Si ya existen los seis archivos de ambas variantes en una Release pública del fork, no repite el proceso. Las versiones preliminares no se importan automáticamente.

Si falta la publicación, incorpora el commit de upstream a master conservando los workflows de este fork. No fuerza actualizaciones de ramas. Si hay conflictos en el código, se detiene sin subir la mezcla. Después llama directamente al workflow de publicación: no depende de que los commits o tags creados con `GITHUB_TOKEN` disparen otra ejecución.

`Publish offline and update release` ejecuta ambas compilaciones y valida que el tag, AssemblyInfo y changelog coincidan. Solo cuando ambas terminan correctamente crea una Release borrador, adjunta los dos EXE, dos ZIP y dos archivos SHA256 y la hace pública. Una publicación parcial se reintenta en la siguiente ejecución. No inventa ni incrementa la versión por su cuenta. También admite tags numéricos publicados manualmente; los tags preliminares se publican como prerelease.

Los cambios de upstream se incorporan antes de compilar; una compilación fallida puede dejar master actualizado, pero no publica una nueva Release. El tag de una nueva Release señala al commit del fork que se compiló. Los conflictos, fuentes incompletas y errores de GitHub aparecen en Actions y requieren atención.

## Activación

Los workflows deben estar integrados en master. Si GitHub mantiene Actions deshabilitado para el fork, habilitarlo en la pestaña Actions. No hacen falta tokens personales: se usa `GITHUB_TOKEN` con lectura para compilar y escritura de contenido solo para sincronizar y publicar. Las reglas de protección de master deben permitir la actualización de Actions; si la bloquean, la sincronización falla sin intentar eludirlas.

Tras la integración, ejecutar `Update from upstream and publish` para comprobar el circuito inmediatamente. La programación de GitHub puede retrasarse y se desactiva por inactividad en repositorios públicos según sus políticas.

El workflow heredado `Mirror to GitLab` conserva su restricción al repositorio original; no se configura ni se utiliza ningún secreto de upstream.
