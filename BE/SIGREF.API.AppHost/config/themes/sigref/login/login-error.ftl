<#-- login-error.ftl -->
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>SIGREF - Error de Login</title>
    <link rel="icon" type="image/svg" href="${url.resourcesPath}/img/LOGO.svg">
    <link rel="stylesheet" href="${url.resourcesPath}/css/style.css">
</head>
<body>
    <div class="login-wrapper">
        <div class="login-container">
            <div class="login-logo">
                <h1>SIGREF</h1>
            </div>

            <div class="login-box">
                <h2 class="titulo">Error de Inicio de Sesión</h2>

                <#-- Mensajes de error de Keycloak -->
                <#if message?has_content>
                    <div class="error-message">
                        <#switch message.summary>
                            <#case "Invalid username or password.">
                                Usuario o contraseña incorrectos. Por favor, verifica tus datos.
                                <#break>
                            <#case "User disabled.">
                                Tu cuenta ha sido desactivada. Contacta al administrador.
                                <#break>
                            <#case "Account temporarily disabled, try again later.">
                                Tu cuenta está temporalmente bloqueada. Intenta más tarde.
                                <#break>
                            <#default>
                                ${message.summary}
                        </#switch>
                    </div>
                </#if>

                <form id="kc-form-login" class="login-form" 
                      action="${url.loginAction}" 
                      method="post">

                    <div class="campo">
                        <label for="username">Nombre de Usuario:</label>
                        <input id="username" class="form-input" name="username" type="text" autofocus value="${(login.username!'')}" autocomplete="username" required/>
                    </div>

                    <div class="campo">
                        <label for="password">Contraseña:</label>
                        <input id="password" class="form-input" name="password" type="password" autocomplete="current-password" required/>
                    </div>

                    <div class="acciones">
                        <input class="btn btn-primary btn-large" name="login" id="kc-login" type="submit" value="Intentar Nuevamente"/>
                    </div>

                    <input type="hidden" id="id-hidden-input" name="credentialId" value="${auth.selectedCredential!''}">
                </form>
            </div>
        </div>
    </div>
</body>
</html>