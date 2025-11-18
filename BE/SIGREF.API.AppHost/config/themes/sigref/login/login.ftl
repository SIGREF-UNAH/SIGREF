<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>SIGREF - Login</title>
    <link rel="icon" type="image/svg" href="${url.resourcesPath}/img/LOGO.svg">
    <link rel="stylesheet" href="${url.resourcesPath}/css/style.css">
</head>
<body>
    <div class="login-wrapper">
        <div class="login-container">
            <div class="login-logo">
                <h1>SIGREF</h1>
                <div class="logos-container">
                    <img
                      src="https://curoc.unah.edu.hn/assets/CUROC/paginas/nuevo-pagina/_resampled/ResizedImageWzMwMCwzMDBd/logos-UNAH-12.png"
                      alt="Ingenieria en Sistemas"
                      class="logo-sistemas"
                    />
                    <img
                      src="https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png"
                      alt="Hospital de Occidente"
                      class="logo-hospital"
                    />
                </div>
            </div>

            <div class="login-box">
                <h2 class="titulo">Inicio de Sesión</h2>

                <!-- Mensajes de error y estados de Keycloak -->
                <#if message?has_content>
                    <#if message.type == 'error'>
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
                                <#case "Invalid grant type.">
                                    Ha ocurrido un error de autenticación. Recarga la página.
                                    <#break>
                                <#default>
                                    ${message.summary}
                            </#switch>
                        </div>
                    <#elseif message.type == 'success'>
                        <div class="success-message">
                            ${message.summary}
                        </div>
                    <#else>
                        <div class="info-message">
                            ${message.summary}
                        </div>
                    </#if>
                </#if>

                <form id="kc-form-login" class="login-form" 
                      action="${url.loginAction}" 
                      method="post">

                    <div class="campo">
                        <label for="username">Usuario:</label>
                        <input id="username" 
                               class="form-input" 
                               name="username" 
                               type="text" 
                               autofocus 
                               value="${(login.username!'')}" 
                               autocomplete="username"
                               required/>
                    </div>

                    <div class="campo">
                        <label for="password">Contraseña:</label>
                        <input id="password" 
                               class="form-input" 
                               name="password" 
                               type="password" 
                               autocomplete="current-password"
                               required/>
                    </div>

                    <div class="acciones">
                        <input class="btn btn-primary btn-large" 
                               name="login" 
                               id="kc-login" 
                               type="submit" 
                               value="Iniciar Sesión"/>
                    </div>

                    <#if realm.rememberMe && !usernameHidden??>
                        <div class="checkbox">
                            <label>
                                <input id="rememberMe" 
                                       name="rememberMe" 
                                       type="checkbox" 
                                       <#if login.rememberMe??>checked</#if>> 
                                Recordarme
                            </label>
                        </div>
                    </#if>

                    <input type="hidden" id="id-hidden-input" name="credentialId" value="${auth.selectedCredential!''}">
                </form>
            </div>
        </div>
    </div>

    <script>
        document.getElementById('kc-form-login').addEventListener('submit', function(e) {
            const submitBtn = document.getElementById('kc-login');
            setTimeout(function() {
                submitBtn.disabled = true;
                submitBtn.value = 'Iniciando...';
            }, 0);
        });
    </script>
</body>
</html>