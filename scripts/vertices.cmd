@echo off
curl --user tigergraph -X GET "%TG_REST_SERVER_URL%/graph/MyGraph/vertices/IP" -H "Authorization: Bearer %TG_Token%"