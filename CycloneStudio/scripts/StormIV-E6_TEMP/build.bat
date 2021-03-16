@ECHO OFF
setlocal
 
set PROJECT=StormIV
:: cesta k vyvojovemu prostredi QUARTUS
set QUARTUSpath=
set PATH=%QUARTUSpath%;%PATH%

:: spusti kompilaci projektu
quartus_sh --flow compile %PROJECT%

:: odstrani pracovni adresare a soubory
rmdir /S /Q db
rmdir /S /Q simulation
rmdir /S /Q incremental_db
copy /Y output\*.sof
rmdir /S /Q output

::vysledny soubor je *.sof
