<#import "template.ftl" as layout>

<@layout.layout title="loginAccountTitle">

    <!-- TÍTULO ESPECÍFICO PARA ESTA PANTALLA -->
    <h2 class="titulo">Error de Inicio de Sesión</h2>

    <!-- MENSAJES DE ERROR -->
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
                    ${message.summary!}
            </#switch>
        </div>
    </#if>

    <!-- FORMULARIO DE REINTENTO -->
    <form id="kc-form-login"
          action="${url.loginAction}"
          method="post">

        <div class="campo">
            <label for="username">Nombre de Usuario:</label>
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
                   id="kc-login"
                   name="login"
                   type="submit"
                   value="Intentar Nuevamente" />
        </div>

        <input type="hidden"
               id="id-hidden-input"
               name="credentialId"
               value="${auth.selectedCredential!''}" />
    </form>

    <!-- LINK A LOGIN -->
    <div class="forgot-password">
        <a href="${url.loginUrl}">Volver al inicio</a>
    </div>

</@layout.layout>
