module cblokBlikac(output wire out_LED0,output wire outLED1,input wire inRESET,input wire inCLK);
//hidden:

wire w1,w3,w4,w5,w6,w7,w8,w9,w10,w11,w2,w12;

cDIVIDE_BY10  b1(.CLK(w1),.RESET(w3),.OUT(w4));
cDIVIDE_BY10  b2(.CLK(w4),.RESET(w3),.OUT(w5));
cDIVIDE_BY10  b3(.CLK(w5),.RESET(w3),.OUT(w6));
cDIVIDE_BY10  b4(.CLK(w6),.RESET(w3),.OUT(w7));
cDIVIDE_BY10  b5(.CLK(w7),.RESET(w3),.OUT(w8));
cDIVIDE_BY10  b6(.CLK(w8),.RESET(w3),.OUT(w9));
cDIVIDE_BY10  b7(.CLK(w9),.RESET(w3),.OUT(w10));
cDIVIDE_BY10  b8(.CLK(w10),.RESET(w3),.OUT(w11));
cNOT b9(.A(w2),.Y(w3));
cNOT b10(.A(w11),.Y(w12));
cout_LED0 b11(.OUT(w12),.out_LED0(out_LED0));
coutLED1 b12(.OUT(w11),.outLED1(outLED1));
cinRESET b13(.inRESET(inRESET),.IN(w2));
cinCLK b14(.inCLK(inCLK),.IN(w1));

endmodule

