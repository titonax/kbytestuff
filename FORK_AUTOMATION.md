# AutomatizaciÃ³n de titonax/kbytestuff

Este fork prepara dos variantes sin firma, con el mismo nÃºmero de versiÃ³n de upstream:

- `WandEnhancer-<version>-offline.exe`: excluye el cÃ³digo de consulta de actualizaciones.
- `WandEnhancer-<version>-update-notifications.exe`: incluye `EnableUpdateNotifications` y consulta las Releases de `titonax/kbytestuff`. Notifica versiones; no descarga ni instala actualizaciones.

Cada variante incluye ademÃ¡s un ZIP con el ejecutable con su nombre original, archivos de ejecuciÃ³n, licencia, changelog y `build-info.json` (repositorio, commit, versiÃ³n y variante), y un archivo SHA256. Se recomienda usar el ZIP para conservar juntos los archivos de ejecuciÃ³n.

## Estado del cÃ³digo recuperado

El antiguo `titonax/Wand-Enhancer` responde HTTP 451 por DMCA y no permite recuperar sus workflows personalizados, historial ni Actions. El chat de referencia solo contiene la solicitud de recuperaciÃ³n. No se ha podido verificar la configuraciÃ³n exacta del antiguo fork.

SÃ­ se recuperÃ³ el workflow `build.yml` del historial disponible de upstream, anterior al commit `428b1addd3dfa5c6160d848ef188f27113e4d2a6` que lo eliminÃ³. Se conservan Windows, Node 22, pnpm 10.17.0, compilaciÃ³n Release, las pruebas de `build.ps1`, la opciÃ³n `EnableUpdateNotifications` y los artefactos sin firma. La matriz ahora genera y conserva ambas variantes por separado.

La versiÃ³n actual de upstream, 2.2.1.0, no compila porque retirÃ³ estas fuentes:

- `WandEnhancer/Core/EnhancerConfig.cs`
- `WandEnhancer/Core/Patching/Shared/ElectronFuseWire.cs`
- `WandEnhancer/Core/Patching/Shared/DiskFusePatch.cs`

El propietario aportÃ³ sus dos ejecutables 2.1.1.0 y se reconstruyeron estas tres clases y dos recursos JavaScript ausentes. [RECOVERY.md](RECOVERY.md) recoge los hashes, herramientas y lÃ­mites de la recuperaciÃ³n. El workflow conserva una comprobaciÃ³n de fuentes completas y no publica sin ambas variantes verificadas.

## Workflows

`Build offline and update executables` permite ejecuciÃ³n manual y comprueba los cambios de master y los PR. Compila y prueba cada variante de forma aislada, y conserva sus artefactos 30 dÃ­as. TambiÃ©n es reutilizable por el proceso de publicaciÃ³n.

`Update from upstream and publish` consulta cada dÃ­a a las 07:23 UTC la Ãºltima Release estable de `k1tbyte/Wand-Enhancer`. TambiÃ©n se ejecuta al instalar o cambiar este workflow en master y se puede iniciar manualmente desde master. Usa el commit exacto del tag, no el master mutable de upstream. Si ya existen los seis archivos de ambas variantes en una Release pÃºblica del fork, no repite el proceso. Las versiones preliminares no se importan automÃ¡ticamente.

Si falta la publicaciÃ³n, incorpora el commit de upstream a master conservando los workflows de este fork. No fuerza actualizaciones de ramas. Si hay conflictos en el cÃ³digo, se detiene sin subir la mezcla. DespuÃ©s llama directamente al workflow de publicaciÃ³n: no depende de que los commits o tags creados con `GITHUB_TOKEN` disparen otra ejecuciÃ³n.

`Publish offline and update release` ejecuta ambas compilaciones y valida que el tag, AssemblyInfo y changelog coincidan. Solo cuando ambas terminan correctamente crea una Release borrador, adjunta los dos EXE, dos ZIP y dos archivos SHA256 y la hace pÃºblica. Una publicaciÃ³n parcial se reintenta en la siguiente ejecuciÃ³n. No inventa ni incrementa la versiÃ³n por su cuenta. TambiÃ©n admite tags numÃ©ricos publicados manualmente; los tags preliminares se publican como prerelease.

Los cambios de upstream se incorporan antes de compilar; una compilaciÃ³n fallida puede dejar master actualizado, pero no publica una nueva Release. El tag de una nueva Release seÃ±ala al commit del fork que se compilÃ³. Los conflictos, fuentes incompletas y errores de GitHub aparecen en Actions y requieren atenciÃ³n.

## ActivaciÃ³n

Los workflows deben estar integrados en master. Si GitHub mantiene Actions deshabilitado para el fork, habilitarlo en la pestaÃ±a Actions. No hacen falta tokens personales: se usa `GITHUB_TOKEN` con lectura para compilar y escritura de contenido solo para sincronizar y publicar. Las reglas de protecciÃ³n de master deben permitir la actualizaciÃ³n de Actions; si la bloquean, la sincronizaciÃ³n falla sin intentar eludirlas.

Tras la integraciÃ³n, ejecutar `Update from upstream and publish` para comprobar el circuito inmediatamente. La programaciÃ³n de GitHub puede retrasarse y se desactiva por inactividad en repositorios pÃºblicos segÃºn sus polÃ­ticas.

El workflow heredado `Mirror to GitLab` conserva su restricciÃ³n al repositorio original; no se configura ni se utiliza ningÃºn secreto de upstream.
