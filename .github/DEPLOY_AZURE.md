# Despliegue Docker en Azure con GitHub Actions

Este flujo construye la imagen Docker de **WorkflowApi** y la despliega en **Azure Web App for Containers**.

## Requisitos en Azure

1. **Azure Container Registry (ACR)**  
   - Crear un ACR (ej. `myacr.azurecr.io`).  
   - En ACR → **Claves de acceso**: activar usuario admin y copiar **Nombre de usuario** y **Contraseña**.

2. **Web App for Containers**  
   - Crear una Web App con sistema operativo **Linux** y publicar en **Docker**.  
   - En **Configuración de Docker**: elegir **Registro de contenedor único**, tu ACR, imagen `workflowapi-api`, etiqueta `latest`.  
   - La app puede arrancar con una imagen dummy; el workflow actualizará la imagen en cada despliegue.

3. **Base de datos**  
   - En Azure usa **Azure Database for MySQL** (o un MySQL externo).  
   - En la Web App → **Configuración** → **Configuración de la aplicación** define la cadena de conexión, por ejemplo:  
     `ConnectionStrings__DefaultConnection` = `Server=tu-servidor.mysql.database.azure.com;Database=WorkflowDb;User=...;Password=...;`  
   - Añade también las variables que necesite la API (JWT, Azure Storage, etc.).

## Secretos en GitHub

En el repositorio: **Settings** → **Secrets and variables** → **Actions** → **New repository secret**.

| Secreto            | Descripción |
|--------------------|-------------|
| `AZURE_CLIENT_ID`  | Application (client) ID de la app de Azure AD usada para OIDC. |
| `AZURE_TENANT_ID`  | Directory (tenant) ID de Azure AD. |
| `AZURE_SUBSCRIPTION_ID` | ID de la suscripción de Azure. |
| `ACR_USERNAME`     | Nombre de usuario del ACR (nombre del registro o usuario admin). |
| `ACR_PASSWORD`     | Contraseña del ACR. |

### Cómo obtener y configurar OIDC (paso a paso)

OIDC permite que GitHub Actions se autentique en Azure **sin guardar contraseñas**: Azure confía en un token que emite GitHub. Solo tienes que crear una “App registration” en Azure, enlazarla a tu repositorio y copiar tres IDs a GitHub.

---

#### Paso 1: Entrar a Azure Portal

1. Ve a [https://portal.azure.com](https://portal.azure.com) e inicia sesión.
2. En el buscador superior escribe **Microsoft Entra ID** (o **Azure Active Directory**) y entra.

---

#### Paso 2: Obtener el Tenant ID (AZURE_TENANT_ID)

1. En el menú izquierdo: **Información general** (Overview).
2. Copia **Id. de directorio (tenant)**.  
   → Ese valor es tu **AZURE_TENANT_ID**.

---

#### Paso 3: Obtener el Subscription ID (AZURE_SUBSCRIPTION_ID)

1. En el buscador del portal escribe **Suscripciones** y entra.
2. Elige la suscripción donde tienes el ACR y la Web App.
3. Copia **Id. de suscripción**.  
   → Ese valor es tu **AZURE_SUBSCRIPTION_ID**.

---

#### Paso 4: Crear la App registration (para OIDC)

1. En **Microsoft Entra ID** → menú izquierdo → **Registros de aplicaciones** (App registrations).
2. **+ Nuevo registro** (New registration).
3. Rellena:
   - **Nombre**: por ejemplo `github-actions-workflow`.
   - **Tipos de cuenta soportados**: “Solo las cuentas de este directorio”.
   - **URI de redirección**: deja en blanco (no hace falta para OIDC).
4. Pulsa **Registrar**.
5. En la página de la aplicación:
   - Copia **Id. de aplicación (cliente)**.  
     → Ese valor es tu **AZURE_CLIENT_ID**.

---

#### Paso 5: Crear la credencial federada (vínculo con GitHub)

1. En la misma App registration → menú izquierdo → **Credenciales** (Certificates & secrets).
2. Arriba verás la pestaña **Credenciales federadas** (Federated credentials). Entra ahí.
3. **+ Agregar credencial** (Add credential).
4. **Escenario de la credencial federada**: elige **Otros** (Other).
5. **Nombre**: ej. `github-main`.
6. **Emisor (Issuer)**: pega exactamente:
   ```text
   https://token.actions.githubusercontent.com
   ```
7. **Identificador del sujeto (Subject)**: debe coincidir con tu repositorio y rama. Formato:
   ```text
   repo:ORGANIZACION_OU_USUARIO/NOMBRE_REPO:ref:refs/heads/main
   ```
   Ejemplos:
   - Repo de un usuario: `repo:juanperez/CRM-BACK:ref:refs/heads/main`
   - Repo de una org: `repo:MiEmpresa/CRM-BACK:ref:refs/heads/main`
8. **Audiencia (Audience)**: pega exactamente:
   ```text
   api://AzureADTokenExchange
   ```
9. **Guardar**.

Con esto ya tienes OIDC configurado: Azure aceptará tokens de GitHub solo para ese repo y rama.

---

#### Paso 6: Dar permisos a la App en Azure (ACR y Web App)

La App registration debe poder usar tu suscripción (o tu grupo de recursos).

1. En el buscador del portal escribe **Grupos de recursos** (o **Suscripciones**).
2. Abre el **grupo de recursos** donde está tu ACR y tu Web App (o la **suscripción** si prefieres dar permiso a todo).
3. Menú izquierdo → **Control de acceso (IAM)**.
4. **+ Agregar** → **Asignación de roles**.
5. **Rol**: elige **Colaborador** (Contributor).
6. **Asignar acceso a**: “Usuario, grupo o entidad de servicio”.
7. **Seleccionar miembros**: busca el nombre de tu App registration (ej. `github-actions-workflow`), selecciónala y **Seleccionar**.
8. **Revisar y asignar**.

---

#### Paso 7: Poner los valores en GitHub (secretos)

1. En GitHub abre tu repositorio (**CRM-BACK**).
2. **Settings** → **Secrets and variables** → **Actions**.
3. **New repository secret** y crea estos secretos:

| Nombre del secreto       | Valor (qué pegar)                          |
|--------------------------|--------------------------------------------|
| `AZURE_CLIENT_ID`        | Id. de aplicación (cliente) del Paso 4     |
| `AZURE_TENANT_ID`        | Id. de directorio del Paso 2               |
| `AZURE_SUBSCRIPTION_ID`  | Id. de suscripción del Paso 3               |
| `ACR_USERNAME`           | Usuario admin del ACR (nombre del registro)|
| `ACR_PASSWORD`           | Contraseña del ACR (claves de acceso)      |

4. **Settings** → **Actions** → **General** → en “Workflow permissions” elige **Read and write permissions** → **Save**.

---

#### Resumen: qué es cada valor

| Dónde lo ves en Azure                    | Secreto / uso                    |
|------------------------------------------|-----------------------------------|
| Entra ID → Información general           | **AZURE_TENANT_ID** (Id. de directorio) |
| Suscripciones → tu suscripción           | **AZURE_SUBSCRIPTION_ID**        |
| App registration → Información general  | **AZURE_CLIENT_ID** (Id. de aplicación)  |
| ACR → Claves de acceso                   | **ACR_USERNAME** y **ACR_PASSWORD**     |

OIDC no usa contraseña de la App: solo **AZURE_CLIENT_ID**, **AZURE_TENANT_ID** y **AZURE_SUBSCRIPTION_ID** más la credencial federada que enlaza el repo de GitHub con esa App.

## Variables del repositorio (opcional)

En **Settings** → **Secrets and variables** → **Actions** → pestaña **Variables** puedes definir:

- **AZURE_WEBAPP_NAME**: nombre de la Web App en Azure (ej. `workflow-api`).  
- **ACR_LOGIN_SERVER**: servidor del ACR (ej. `myacr.azurecr.io`).

Si no las defines, el workflow usa por defecto `workflow-api` y `yourregistry.azurecr.io` (debes cambiar al menos `ACR_LOGIN_SERVER` en el YAML o en variables).

## Ejecución

- **Automática**: en cada `push` a la rama `main`.  
- **Manual**: en la pestaña **Actions** → **Build and Deploy Docker to Azure** → **Run workflow**.

## Alternativa: login con Client Secret

Si prefieres no usar OIDC, puedes usar **Azure/login** con client secret:

- Añade el secreto `AZURE_CREDENTIALS` con el JSON de la entidad de servicio (como en la [documentación de azure/login](https://github.com/Azure/login#configure-deployment-credentials)).  
- En el workflow sustituye el paso "Azure Login (OIDC)" por el login con `creds` y usa ese mismo bloque en ambos jobs.
