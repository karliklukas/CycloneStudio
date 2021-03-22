module cCOUNTER( input wire CLK , input wire RESET, output wire [3:0] COUNT);

reg [3:0] count_reg;
always @ (posedge CLK or negedge RESET) 
begin
  if (~RESET) 
    begin
      count_reg <= 'b0;
    end
  else 
   begin
    count_reg <= count_reg + 1'b1;
  end

end

assign COUNT = count_reg;
endmodule
