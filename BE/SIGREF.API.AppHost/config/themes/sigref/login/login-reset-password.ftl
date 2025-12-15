<#import "template.ftl" as layout>

<@layout.layout title="Recupera tu Contraseña">

<form id="kc-reset-password-form"
      class="login-form"
      action="${url.loginAction}"
      method="post">

    <div class="campo">
        <label for="username">${msg("Email o Usuario")}</label>
        <input class="form-input"
               type="text"
               id="username"
               name="username"
               autofocus />
    </div>

    <div class="acciones">
        <button class="btn btn-primary btn-large" type="submit">
            ${msg("Enviar")}
        </button>
    </div>

</form>

<div class="forgot-password">
    <a href="${url.loginUrl}">${msg("Volver a Login")}</a>
</div>

</@layout.layout>
