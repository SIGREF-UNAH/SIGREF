<#import "template.ftl" as layout>

<@layout.layout title="Inicio de Sesion">

<!-- MENSAJES DE ESTADO -->
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
                <#default>
                    Error: Revise sus credenciales o contacte al administrador.
            </#switch>
        </div>
    <#elseif message.type == 'success'>
        <div class="success-message">${message.summary}</div>
    <#else>
        <div class="info-message">${message.summary}</div>
    </#if>
</#if>

<!-- FORMULARIO DE LOGIN -->
<form id="kc-form-login"
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
               required />
    </div>

    <div class="campo">
        <label for="password">Contraseña:</label>
        <input id="password"
               class="form-input"
               name="password"
               type="password"
               autocomplete="current-password"
               required />
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

    <input type="hidden"
           id="id-hidden-input"
           name="credentialId"
           value="${auth.selectedCredential!''}" />

</form>

<!-- LINK OLVIDÉ MI CONTRASEÑA -->
<div class="forgot-password">
    <a href="${url.loginResetCredentialsUrl}">¿Olvidaste tu contraseña?</a>
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

</@layout.layout>
