@echo off
curl --location --request GET "%TG_SERVER_URL%echo" -H "Authorization: Bearer %TG_TOKEN%" -H "Origin: https://www.foo.com" -i -H "Access-Control-Request-Method: GET" -H "Access-Control-Request-Headers: X-Requested-With" -v