@ECHO OFF

set PROJECT=DE0Nano
:: cesta k vyvojovemu prostredi QUARTUS
set QUARTUSpath=e:\intelFPGA_lite\18.1\quartus\bin64

::quartus_pgm
:: %PROJECT%

setx path "%path%;%QUARTUSpath%"
quartus_pgm –c usbblasterII –m jtag –o bpv;design.pof 

