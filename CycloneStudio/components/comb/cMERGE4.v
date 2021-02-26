module cMERGE4(input wire BUS0, input wire BUS1, input wire BUS2, input wire BUS3, output wire BUS[3:0]);
assign BUS = {BUS3, BUS2, BUS1, BUS0};
endmodule
