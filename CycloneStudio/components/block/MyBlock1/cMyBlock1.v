module cMyBlock1(input wire KEY1,input wire Cst1,output wire out1,output wire LED1);
//hidden: KEY1,LED1

wire w1,w2,w3;

cKEY1 b1(.KEY1(KEY1),.OUT(w1));
cCst1 b2(.Cst1(Cst1),.IN(w2));
cAND2 b3(.A(w1),.B(w2),.Y(w3));
cout1 b4(.OUT(w3),.out1(out1));
cLED1 b5(.IN(w3),.LED1(LED1));

endmodule

