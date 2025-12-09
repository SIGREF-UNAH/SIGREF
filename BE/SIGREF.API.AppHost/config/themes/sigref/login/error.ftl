<#import "template.ftl" as layout>

<@layout.layout title="errorTitle">

    <!-- MENSAJE PRINCIPAL -->
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

    <!-- DETALLES OPCIONALES -->
    <#if error_description??>
        <div class="error-details" style="margin-top:10px; color:#333; font-size:14px;">
            <small>${error_description!}</small>
        </div>
    </#if>

    <!-- BOTÓN PARA VOLVER AL LOGIN -->
    <div class="acciones" style="margin-top:25px;">
        <a class="btn btn-primary btn-large"
           href="<#if url.loginUrl??>${url.loginUrl}<#else>${url.realmUrl}</#if>">
            Volver al inicio de sesión
        </a>
    </div>

</@layout.layout>
