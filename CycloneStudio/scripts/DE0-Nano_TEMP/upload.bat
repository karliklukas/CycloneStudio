@ECHO OFF
setlocal
 
set PROJECT=DE0Nano
:: cesta k vyvojovemu prostredi QUARTUS
set QUARTUSpath=
set PATH=%QUARTUSpath%;%PATH%

:: naprogramuje vyvojovy kit
quartus_pgm -c usb-blaster -m jtag -o p;%PROJECT%.sof 

