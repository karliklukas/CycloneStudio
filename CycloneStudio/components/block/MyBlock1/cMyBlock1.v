module cMyBlock1(input wire poiu,output wire kjhkj,input wire KEY1,output wire LED1);
//hidden: KEY1,LED1

wire w2,w3,w1;

cpoiu b1(.poiu(poiu),.IN(w2));
ckjhkj b2(.OUT(w3),.kjhkj(kjhkj));
cKEY1 b3(.KEY1(KEY1),.OUT(w1));
cAND3 b4(.A(w1),.B(w2),.C(w2),.Y(w3));
cLED1 b5(.IN(w3),.LED1(LED1));

endmodule

