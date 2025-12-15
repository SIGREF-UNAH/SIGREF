<#import "template.ftl" as layout>

<@layout.layout title="authenticatorOTPTitle">

    <!-- Mensajes -->
    <#if message?has_content>
        <#if message.type == 'error'>
            <div class="error-message">${message.summary}</div>
        <#elseif message.type == 'success'>
            <div class="success-message">${message.summary}</div>
        <#else>
            <div class="info-message">${message.summary}</div>
        </#if>
    </#if>

    <form id="kc-otp-login-form"
          action="${url.loginAction}"
          method="post">

        <div class="campo">
            <label for="otp">${msg("otpCode")}</label>
            <input id="otp"
                   class="form-input"
                   name="otp"
                   type="text"
                   autocomplete="one-time-code"
                   autofocus
                   required />
        </div>

        <div class="acciones">
            <button class="btn btn-primary btn-large" type="submit">
                ${msg("doSubmit")}
            </button>
        </div>

    </form>

    <div class="forgot-password">
        <a href="${url.loginUrl}">Cancelar y volver</a>
    </div>

</@layout.layout>
