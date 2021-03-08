module cMyBlock1(input wire SWITCH1,input wire SWITCH2,input wire SWITCH3,input wire SWITCH4,input wire CustIN,output wire CustOut);
//hidden: SWITCH1,SWITCH2,SWITCH3,SWITCH4

wire w1,w2,w3,w4,w5;

cSWITCH b1(.SWITCH1(SWITCH1),.SWITCH2(SWITCH2),.SWITCH3(SWITCH3),.SWITCH4(SWITCH4),.SW1(w1),.SW2(w2),.SW3(w3),.SW4());
cOR4 b2(.A(w1),.B(w2),.C(w3),.D(w4),.Y(w5));
cCustIN b3(.CustIN(CustIN),.IN(w4));
cCustOut b4(.OUT(w5),.CustOut(CustOut));

endmodule

