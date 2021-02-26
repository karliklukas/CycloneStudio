module cMERGE2(input wire BUS0, input wire BUS1, output wire BUS[1:0]);
assign BUS = {BUS1, BUS0};
endmodule
