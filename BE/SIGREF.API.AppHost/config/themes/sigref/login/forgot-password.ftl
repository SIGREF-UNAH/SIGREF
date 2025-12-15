<#import "template.ftl" as layout>

<@layout.layout title="emailForgotTitle">

<form id="kc-reset-password-form" action="${url.loginAction}" method="post">

    <!-- MENSAJES -->
    <#if message?has_content>
        <#if message.type == 'error'>
            <div class="error-message">${message.summary}</div>
        <#elseif message.type == 'success'>
            <div class="success-message">${message.summary}</div>
        <#else>
            <div class="info-message">${message.summary}</div>
        </#if>
    </#if>

    <!-- INPUT DE USUARIO / EMAIL -->
    <div class="campo">
        <label for="username">${msg("usernameOrEmail")}</label>
        <input class="form-input"
               type="text"
               id="username"
               name="username"
               autofocus
               required />
    </div>

    <!-- BOTÓN -->
    <div class="acciones">
        <button class="btn btn-primary btn-large" type="submit">
            ${msg("doSubmit")}
        </button>
    </div>

</form>

<!-- VOLVER AL LOGIN -->
<div class="forgot-password">
    <a href="${url.loginUrl}">
        ${msg("backToLogin")}
    </a>
</div>

</@layout.layout>
