@echo off
curl --user tigergraph -X GET "%TG_GSQL_SERVER_URL%/gsqlserver/gsql/schema/?graph=MyGraph"