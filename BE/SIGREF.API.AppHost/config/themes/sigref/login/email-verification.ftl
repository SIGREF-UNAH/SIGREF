<#import "template.ftl" as layout>

<@layout.layout title="emailVerifyTitle">

    <!-- MENSAJES DE ESTADO -->
    <#if message?has_content>
        <#if message.type == 'error'>
            <div class="error-message">${message.summary}</div>
        <#elseif message.type == 'success'>
            <div class="success-message">${message.summary}</div>
        <#else>
            <div class="info-message">${message.summary}</div>
        </#if>
    </#if>

    <!-- TEXTO PRINCIPAL -->
    <div style="text-align:center; margin-bottom:20px; color:#333; font-size:15px;">
        <p>Hemos enviado un correo a tu dirección registrada.</p>
        <p>Sigue las instrucciones para completar el proceso.</p>
    </div>

    <!-- OPCIÓN: reenviar correo si Keycloak lo soporta -->
    <#if resendAllowed?? && resendAllowed>
        <form action="${url.loginAction}" method="post">
            <div class="acciones">
                <button class="btn btn-primary btn-large" type="submit">
                    Reenviar correo
                </button>
            </div>
        </form>
    </#if>

    <!-- VOLVER AL LOGIN -->
    <div class="forgot-password">
        <a href="${url.loginUrl}">
            Volver al inicio de sesión
        </a>
    </div>

</@layout.layout>
