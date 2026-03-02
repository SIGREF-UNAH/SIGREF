<#-- TEMPLATE BASE DEL THEME -->

<#macro layout title>

<!DOCTYPE html>
<html lang="${(locale!'es')}">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>${msg(title)!}</title>

    <link rel="icon" type="image/svg"
          href="https://raw.githubusercontent.com/MichaelGald/assets/refs/heads/main/LOGO.svg">

    <link rel="stylesheet" href="${url.resourcesPath}/css/style.css" />

    <#if styles??>
        <#list styles as style>
            <link rel="stylesheet" href="${url.resourcesPath}/${style}" />
        </#list>
    </#if>
</head>

<body>

<div class="login-wrapper">
    <div class="login-container">

        <div class="login-logo">
            <h1>SIGREF</h1>

            <div class="logos-container">
                
            </div>
        </div>

        <div class="login-box">
            <h2 class="titulo">${msg(title)!}</h2>

            <section class="login-form">
                <#nested/>   <!-- AQUI KEYCLOAK INSERTA EL CONTENIDO -->
            </section>
        </div>

    </div>
</div>

</body>
</html>

</#macro>
