@ECHO OFF
 
set PROJECT=DE0Nano
:: cesta k vyvojovemu prostredi QUARTUS
set QUARTUSpath=e:\intelFPGA_lite\18.1\quartus\bin64

setx path "%path%;%QUARTUSpath%"
quartus_sh --flow compile %PROJECT%

