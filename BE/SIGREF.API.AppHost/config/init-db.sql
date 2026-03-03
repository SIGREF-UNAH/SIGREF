-- =============================================================
-- Inicializacion de Bases de Datos del Sistema SIGREF
-- =============================================================

-- Usamos esta tecnica para que el script no falle si las dbs ya existen
SELECT 'CREATE DATABASE hapi' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'hapi')\gexec
SELECT 'CREATE DATABASE keycloak' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec
SELECT 'CREATE DATABASE sigref' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'sigref')\gexec
SELECT 'CREATE DATABASE hangfire' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'hangfire')\gexec

-- Opcional: Asignar privilegios si usas un usuario especifico
-- ALTER DATABASE hapi OWNER TO mi_usuario_sigref;