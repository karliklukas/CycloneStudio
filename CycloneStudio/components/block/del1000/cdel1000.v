module cdel1000(input wire vstup,output wire vystup);
//hidden:

wire w3,w5,w1,w2,w4;

cDIVIDE_BY10  b1(.CLK(w3),.RESET(w5),.OUT(w1));
cDIVIDE_BY10  b2(.CLK(w1),.RESET(w5),.OUT(w2));
cDIVIDE_BY10  b3(.CLK(w2),.RESET(w5),.OUT(w4));
cvstup b4(.vstup(vstup),.IN(w3));
cvystup b5(.OUT(w4),.vystup(vystup));
cZERO b6(.Y(w5));

endmodule

