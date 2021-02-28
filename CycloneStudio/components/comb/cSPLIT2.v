module cSPLIT2(input wire [1:0] BUS, output wire BUS0, output wire BUS1);
assign BUS0 = BUS[0];
assign BUS1 = BUS[1]; 
endmodule
