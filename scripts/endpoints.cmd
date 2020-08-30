@echo off
curl --location --request GET "%TG_SERVER_URL%endpoints?builtin=true&dynamic=true&static=true" -H "Authorization: Bearer %TG_TOKEN%"