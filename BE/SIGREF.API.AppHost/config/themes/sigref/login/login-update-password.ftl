<#import "template.ftl" as layout>

<@layout.layout title="updatePasswordTitle">

<form id="kc-passwd-update-form"
      class="login-form"
      action="${url.loginAction}"
      method="post">

    <div class="campo">
        <label for="password-new">${msg("passwordNew")}</label>
        <input id="password-new"
               name="password-new"
               type="password"
               class="form-input"
               autofocus />
    </div>

    <div class="campo">
        <label for="password-confirm">${msg("passwordConfirm")}</label>
        <input id="password-confirm"
               name="password-confirm"
               type="password"
               class="form-input" />
    </div>

    <div class="acciones">
        <button class="btn btn-primary btn-large" type="submit">
            ${msg("doSubmit")}
        </button>
    </div>
</form>

</@layout.layout>
