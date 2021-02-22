module cMyBlock1(output wire LED1,input wire CustIn1,output wire OUT_PIN1);
//hidden: LED1,OUT_PIN1

wire w3,w1,w2;

cLED1 b1(.IN(w3),.LED1(LED1));
cCustIn1 b2(.CustIn1(CustIn1),.IN(w1));
cAND2 b3(.A(w1),.B(w2),.Y(w3));
cZERO b4(.Y(w2));
cOUT_PIN1 b5(.OUT(w3),.OUT_PIN1(OUT_PIN1));

endmodule

