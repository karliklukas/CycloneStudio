module cDIVIDE_BY10 ( input wire CLK , input wire RESET, output wire OUT);
 
parameter WIDTH = 3;    // Width of the register required (2^WIDTH > N)
parameter N = 5;         // We will divide by 2N 

 
reg [WIDTH-1:0] r_reg;
wire [WIDTH-1:0] r_nxt;
reg clk_track;
 
always @(posedge CLK  or posedge RESET)
 
begin
  if (RESET)
     begin
        r_reg <= 0;
	clk_track <= 1'b0;
     end
 
  else if (r_nxt == N)
 	   begin
	     r_reg <= 0;
	     clk_track <= ~clk_track;
	   end
 
  else 
      r_reg <= r_nxt;
end
 
 assign r_nxt = r_reg+1;   	      
 assign OUT = clk_track;
endmodule
