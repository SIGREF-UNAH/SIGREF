<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>SIGREF - Error</title>
    <link rel="icon" type="image/svg" href="${url.resourcesPath}/img/LOGO.svg">
    <link rel="stylesheet" href="${url.resourcesPath}/css/style.css">
</head>
<body>
    <div class="login-wrapper">
        <div class="login-container">
            <div class="login-box">
                <h2 class="titulo">Ha ocurrido un error</h2>

                <!-- Mostrar mensaje de error de Keycloak -->
                <#if message?? && message.type == 'error'>
                    <div class="error-message">
                        <strong>Error:</strong> ${message.summary!}
                    </div>
                <#elseif message??>
                    <div class="error-message">
                        ${message.summary!}
                    </div>
                <#elseif error??>
                    <div class="error-message">
                        ${error!}
                    </div>
                <#else>
                    <div class="error-message">
                        Ocurrió un error desconocido.
                    </div>
                </#if>

                <!-- Mostrar detalles adicionales si est�n disponibles -->
                <#if error_description??>
                    <div class="error-details">
                        <small>${error_description!}</small>
                    </div>
                </#if>

                <!-- Enlace seguro para volver al login del mismo realm -->
                <div class="acciones">
                    <a href="<#if url.loginUrl??>${url.loginUrl}<#else>${url.realmUrl}</#if>" class="btn btn-primary btn-large">
                        Volver al inicio de sesión
                    </a>
                </div>
            </div>
        </div>
    </div>
</body>
</html>