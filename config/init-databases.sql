-- Create databases for Keycloak, HAPI FHIR, and SIGREF if they don't exist
SELECT 'CREATE DATABASE keycloak'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec

SELECT 'CREATE DATABASE hapi'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'hapi')\gexec

SELECT 'CREATE DATABASE sigref'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'sigref')\gexec
