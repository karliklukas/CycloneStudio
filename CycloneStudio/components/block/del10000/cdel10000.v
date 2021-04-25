module cdel10000(input wire vstup,output wire vystup);
//hidden:

wire w1,w6,w2,w3,w4,w5;

cDIVIDE_BY10  b1(.CLK(w1),.RESET(w6),.OUT(w2));
cDIVIDE_BY10  b2(.CLK(w2),.RESET(w6),.OUT(w3));
cDIVIDE_BY10  b3(.CLK(w3),.RESET(w6),.OUT(w4));
cDIVIDE_BY10  b4(.CLK(w4),.RESET(w6),.OUT(w5));
cvstup b5(.vstup(vstup),.IN(w1));
cvystup b6(.OUT(w5),.vystup(vystup));
cZERO b7(.Y(w6));

endmodule

