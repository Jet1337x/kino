@echo off

set script=%1
set dest=%2
set config=%3

python %script% %dest% %config%
