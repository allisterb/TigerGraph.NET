@echo off
 curl -X GET "%TG_REST_SERVER_URL%/graph/MyGraph/edges/IP/194.74.87.144" -H "Authorization: Bearer %TG_Token%"