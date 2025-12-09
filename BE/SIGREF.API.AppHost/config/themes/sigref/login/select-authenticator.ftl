<#import "template.ftl" as layout>

<@layout.layout title="authenticatorSelectTitle">

    <#if message?has_content>
        <#if message.type == 'error'>
            <div class="error-message">${message.summary}</div>
        <#elseif message.type == 'success'>
            <div class="success-message">${message.summary}</div>
        <#else>
            <div class="info-message">${message.summary}</div>
        </#if>
    </#if>

    <form id="kc-select-authenticator-form"
          action="${url.loginAction}"
          method="post">

        <div class="campo">
            <label>${msg("chooseAuthenticator")}</label>

            <#list authenticators as authenticator>
                <div style="margin-bottom: 12px;">
                    <label>
                        <input type="radio"
                               name="authenticationExecution"
                               value="${authenticator.authExecId}"
                               required />
                        ${authenticator.displayName!}
                    </label>
                </div>
            </#list>
        </div>

        <div class="acciones">
            <button class="btn btn-primary btn-large" type="submit">
                ${msg("doContinue")}
            </button>
        </div>

    </form>

    <div class="forgot-password">
        <a href="${url.loginUrl}">Volver</a>
    </div>

</@layout.layout>
