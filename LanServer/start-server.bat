set FLASK_APP=main.py
call activate WebServer

rem debugging (non-public)
rem call python -m flask run

rem public server
call python -m flask run --host=0.0.0.0