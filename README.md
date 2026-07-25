# Antigravity Integration Tool (`com.antigravity.ide`)

 Paquete de integración oficial para habilitar **Antigravity IDE** como el editor de código C# externo predeterminado en **Unity Editor**.

Ofrece autocompletado e IntelliSense completo mediante la generación de archivos de solución (`.sln`) y proyectos (`.csproj`), sincronización automática de assets y apertura fluida de scripts directamente en la línea y columna correspondientes.

---

## 🚀 Características

- ⚡ **Integración Nativa con Unity**: Registrado a través del paquete `Unity.CodeEditor` (`IExternalCodeEditor`).
- 🎯 **Apertura Precisa de Scripts**: Abre scripts de C# desde el Project Window o la Consola de Unity directamente en la línea y columna del error/código.
- 💡 **Sincronización Inteligente de Soluciones**: Genera y actualiza automáticamente los archivos `.sln` y `.csproj` al agregar, mover o eliminar scripts en Unity.
- 🔍 **Detección Automática de Instalación**: Localiza automáticamente el ejecutable de Antigravity IDE en Windows, macOS y Linux.
- 🛠️ **Menú Integrado en Unity**: Opciones de acceso rápido desde la barra superior (`Antigravity/...`).

---

## 📋 Prerrequisitos y Requisitos del Sistema

Antes de instalar y configurar el paquete, asegúrate de contar con los siguientes elementos:

1. **Unity Editor**:
   - Versión **2021.3 LTS** o superior.
   - Módulo de compilación C# / .NET instalado (incluido por defecto al instalar Unity Editor mediante Unity Hub).

2. **Antigravity IDE / Antigravity CLI (`agy`)**:
   - Instalado en tu equipo (disponible para Windows, macOS o Linux).

3. **Git** *(Opcional / Recomendado)*:
   - Requerido en el sistema si deseas instalar el paquete directamente desde una URL de Git mediante el Unity Package Manager.

4. **Sistemas Operativos Compatibles**:
   - **Windows**: Windows 10 / 11 (64-bit).
   - **macOS**: macOS 10.15 (Catalina) o posterior (compatible con Intel y Apple Silicon).
   - **Linux**: Distribuciones populares de Linux (Ubuntu, Debian, Fedora, Arch, etc.).

---

## 🛠️ Instalación

Puedes instalar el paquete en tu proyecto de Unity utilizando cualquiera de los siguientes métodos:

### Método 1: Desde el Unity Package Manager (Recomendado)

1. Abre tu proyecto en **Unity**.
2. Ve al menú superior y selecciona **Window > Package Manager**.
3. Haz clic en el icono **`+`** (esquina superior izquierda) y selecciona **Add package from git URL...**.
4. Pega la siguiente URL:
   ```text
   https://github.com/Wannacr7/AntigravityIntegrationTool.git?path=Packages/com.antigravity.ide
   ```
5. Haz clic en **Add**. Unity descargará e instalará el paquete automáticamente.

---

### Método 2: Editando `Packages/manifest.json`

Abre el archivo `Packages/manifest.json` de tu proyecto Unity y añade la dependencia dentro del bloque `"dependencies"`:

```json
{
  "dependencies": {
    "com.antigravity.ide": "https://github.com/Wannacr7/AntigravityIntegrationTool.git?path=Packages/com.antigravity.ide",
    "...": "..."
  }
}
```

---

### Método 3: Instalación Local (Embedded Package)

1. Clona o descarga este repositorio.
2. Copia la carpeta `Packages/com.antigravity.ide` dentro de la carpeta `Packages/` de tu proyecto Unity.
3. Unity detectará e importará el paquete automáticamente al enfocar la ventana.

---

## ⚙️ Configuración

Una vez instalado el paquete, debes seleccionar **Antigravity IDE** como tu editor de código externo en Unity:

### Opción A: A través del menú rápido de Antigravity
En la barra de menú superior de Unity, selecciona:
> **Antigravity > Set as Active Unity Editor**

### Opción B: A través de las Preferencias de Unity
1. Ve a **Edit > Preferences...** *(Windows/Linux)* o **Unity > Preferences...** *(macOS)*.
2. Selecciona la pestaña **External Tools**.
3. En el desplegable **External Script Editor**, selecciona **Antigravity IDE**.
   - *Nota*: Si Antigravity IDE no aparece en la lista automáticamente, haz clic en **Browse...** y selecciona el ejecutable de Antigravity IDE (`Antigravity IDE.exe`, `agy.exe`, o el archivo `.app` en macOS).

---

## 📖 Modo de Uso

### 1. Abrir Scripts de C#
- Haz **doble clic** en cualquier script `.cs` dentro de la ventana *Project* de Unity para abrirlo directamente en Antigravity IDE.
- Haz **doble clic en un mensaje de error o advertencia** de la *Console* de Unity para ir directamente a la línea donde ocurrió la excepción o log.

### 2. Abrir el Proyecto Completo
Puedes abrir el directorio completo del proyecto Unity en Antigravity IDE desde la barra de menú:
> **Antigravity > Open Project in Antigravity IDE**

### 3. Regenerar Archivos de Solución C# (`.sln` / `.csproj`)
Si agregas nuevas librerías, packages o requieres refrescar las referencias de IntelliSense:
- **Desde el menú**: Selecciona **Antigravity > Regenerate C# Solution Files**.
- **Desde Preferencias**: En **Edit > Preferences > External Tools**, selecciona Antigravity IDE y presiona el botón **Regenerate C# Solution Files**.

---

## 🔍 Rutas de Detección Automática

El paquete busca automáticamente Antigravity IDE en las siguientes rutas por defecto según el sistema operativo:

- **Windows**:
  - `%LOCALAPPDATA%\Programs\Antigravity IDE\Antigravity IDE.exe`
  - `%LOCALAPPDATA%\Programs\Antigravity IDE\bin\antigravity-ide.cmd`
  - `%LOCALAPPDATA%\agy\bin\agy.exe`
  - `%ProgramFiles%\Antigravity IDE\Antigravity IDE.exe`
- **macOS**:
  - `/Applications/Antigravity IDE.app`
  - `~/Applications/Antigravity IDE.app`
- **Linux**:
  - `/usr/bin/antigravity-ide`
  - `/usr/local/bin/antigravity-ide`
  - `/snap/bin/antigravity-ide`

Si tu ejecutable se encuentra en una ubicación distinta, se guardará tu ruta personalizada en las preferencias del editor.

---

## 📄 Licencia

Este proyecto está bajo la licencia [GNU General Public License v3.0](LICENSE).