module cSPLIT4(input wire [3:0] BUS, output wire BUS0, output wire BUS1, output wire BUS2, output wire BUS3);
assign BUS0 = BUS[0];
assign BUS1 = BUS[1];
assign BUS2 = BUS[2];
assign BUS3 = BUS[3];
endmodule
