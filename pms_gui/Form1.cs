/**
 * @Author     = lianghawe
 * @Date       = 2019/5/9 21:12
 * @Desprition = A simple Process Management System
 * 
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace pms_gui
{
    /**
     *    FIRST 
     *           DATA STRUCTRUE
     * 
     * */


    /// <summary>
    /// It use structure to represent the contents except ID.
    /// </summary>
    /// 
    public struct pcb_contents
    {
        public string status;
        public int priority;
        public int runtime;
        public int io_time;
        public int io_duration;
        public string io_type;
        public string storage_addresses;
        public string site_information;
        public string management_information;


    }

    /// <summary>
    /// It used to Process Scheduling.
    /// </summary>
    /// 
    struct three_times
    {
        public double cpu_time1;
        public double io_time;
        public double cpu_time2;
    }

 

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Some static variables used in the functions.
        /// 
        /// </summary>
        public static Dictionary<int, pcb_contents> PCB = new Dictionary<int, pcb_contents>();
        static Dictionary<int, three_times> three_time = new Dictionary<int, three_times>();
        public static Queue<int> print_queue = new Queue<int>();
        public static Queue<int> color_printing_queue = new Queue<int>();
        public static Queue<int> ready_queue_1 = new Queue<int>();
        public static Queue<int> ready_queue_2 = new Queue<int>();
        public static int running_id = -1;
        public static int slice_times = 0;
        public static double priority_1_slice;
        public static double priority_2_slice;


        /**
         *    SECOND  
         *              PROCESSING OF INPUT
         * 
         * */


        /// <summary>
        /// After users click the button, it creates the number of processes based on user input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Visible = true;
            button2.Visible = true;
            dataGridView1.RowCount = Convert.ToInt32(textBox1.Text);

            button1.Enabled = false;

            for (int i = 0; i < dataGridView1.RowCount; i++)
                dataGridView1[0, i].Value = Convert.ToString(i);

            //
            
            //this.Size = new System.Drawing.Size(390, 280);

        }

        /// <summary>
        /// After users click the button, it adds the process to the ready queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //
            if (param_legal())
            {
                return;
            }

            //
            get_time_slices();

            //
            print_title();

            /// Create the PCB and add to ready queue.
            for (int j = 0; j < dataGridView1.RowCount; j++)
            {
                pcb_contents pcb1;
                pcb1.status = "new";
                pcb1.priority = 1;
                pcb1.runtime = Convert.ToInt32(dataGridView1[1, j].Value);
                pcb1.io_time = Convert.ToInt32(dataGridView1[2, j].Value);
                pcb1.io_duration = Convert.ToInt32(dataGridView1[3, j].Value);
                pcb1.io_type = Convert.ToString(dataGridView1[4, j].Value);
                pcb1.storage_addresses = "";
                pcb1.site_information = "";
                pcb1.management_information = "";
                PCB.Add(j, pcb1); 

                //
                ready_queue_1.Enqueue(j);

                //
                button2.Enabled = false;
            }

            // 
            //this.Size = new System.Drawing.Size(700, 300);
            for(int key = 0; key<dataGridView1.RowCount; key++)
                state_switching(key, "ready");


            textBox2.AppendText(System.Environment.NewLine + "====AFTER CREATED======" + System.Environment.NewLine);

            print_state_switching();

            //
            computer_three_times();
            multi_level_feedback_queue();
        }


        /// <summary>
        /// Change the state
        /// </summary>
        /// <param name="id"></param>
        /// <param name="current_state"></param>
        void state_switching(int id, string current_state)
        {
            pcb_contents pc = PCB[id];
            pc.status = current_state;
            PCB[id] = pc;
        }


        /// <summary>
        /// Check if the process parameters are legal.
        /// </summary>
        bool param_legal()
        {

            // It makes the input complete
            for (int i = 0; i < dataGridView1.ColumnCount - 1; i++)
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    var cellValue = dataGridView1[i, j].Value;

                    if (cellValue == null || cellValue == DBNull.Value
                    || String.IsNullOrWhiteSpace(cellValue.ToString()))
                    {
                        MessageBox.Show("Please enter sufficient value");
                        return true;
                    }
                }

            bool able = false;

            // Make runtime >= io_time + io_duration
            for (int row_number = 0; row_number < dataGridView1.RowCount; row_number++)
            {
                if (Convert.ToString(dataGridView1[1, row_number].Value) != "" && Convert.ToString(dataGridView1[2, row_number].Value) != "" && Convert.ToString(dataGridView1[3, row_number].Value) != "")
                    if (Convert.ToInt32(dataGridView1[1, row_number].Value) < Convert.ToInt32(dataGridView1[3, row_number].Value) + Convert.ToInt32(dataGridView1[2, row_number].Value))
                    {
                        able = true;
                        MessageBox.Show("Time error 1!");

                    }


                // Make it not be (0, 0, 0) 
                if (Convert.ToInt32(dataGridView1[1, row_number].Value) == 0 && Convert.ToInt32(dataGridView1[3, row_number].Value) == 0 && Convert.ToInt32(dataGridView1[2, row_number].Value) == 0)
                {
                    able = true;
                    MessageBox.Show("Time error 2!");

                }
            }
            

            return able;

        }

        /// <summary>
        /// Make the input number in [0, 9] or Backspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8) || Convert.ToInt32(textBox1.Text) == 0)
            {
                e.Handled = false;
            }
            else
            {
                MessageBox.Show("You need to input a positive integer!");
                e.Handled = true;
            }

            
        }


        /// <summary>
        /// Make the input legal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cells_KeyPress(object sender, PreviewKeyDownEventArgs e) //自定义事件
        {
            
            //
            if ((e.KeyValue >= '0' && e.KeyValue <= '9') || (e.KeyValue == 8))
            {
                //e.Handled = false;
            }
            else
            {
                MessageBox.Show("You need to input a integer!");
                //e.Handled = true;
            }

            // Make runtime >= io_time + io_duration
            int row_number = dataGridView1.CurrentCell.RowIndex;
            if (Convert.ToString(dataGridView1[1, row_number].Value) != "" && Convert.ToString(dataGridView1[2, row_number].Value) != "" && Convert.ToString(dataGridView1[3, row_number].Value) != "")
                if (Convert.ToInt32(dataGridView1[1, row_number].Value) < Convert.ToInt32(dataGridView1[3, row_number].Value) + Convert.ToInt32(dataGridView1[2, row_number].Value))
                {
                    MessageBox.Show("Time error 1!");
                    button2.Visible = false;
                }
                else
                    button2.Visible = true;



            // Make it not be (0, 0, 0) 
            if (dataGridView1[1, row_number].Value == "0" && dataGridView1[2, row_number].Value == "0" && dataGridView1[3, row_number].Value == "0")
            {
                MessageBox.Show("Time error 2!");
                button2.Visible = false;
            }

            else
                button2.Visible = true;



        }

        /**
         *      THIRD 
         *              IMPLEMENT ALGORITHM
         * 
         * */


        /// <summary>
        /// Get the three_time for the Schedule
        /// 
        /// </summary>
        void computer_three_times()
        {
            foreach(var key in PCB.Keys)
            {
                three_times tt;
                
                tt.cpu_time1 = PCB[key].io_time;
                tt.io_time = PCB[key].io_duration;
                tt.cpu_time2 = PCB[key].runtime - PCB[key].io_duration - PCB[key].io_time;

                if (PCB[key].io_time == 0)
                {
                    tt.cpu_time1 = PCB[key].runtime;
                    tt.cpu_time2 = 0;
                }
                three_time.Add(key, tt);
            }
        }


        /// <summary>
        /// Read configuration file to get slice
        /// </summary>
        void get_time_slices()
        {
            XmlDocument config = new XmlDocument();
            config.Load("Configuration.config");

            var slices = config.SelectSingleNode("configuration");
            priority_1_slice = Convert.ToDouble(slices.SelectSingleNode("priority_1").InnerText);
            priority_2_slice = Convert.ToDouble(slices.SelectSingleNode("priority_2").InnerText);

        }


        /// <summary>
        /// One loop is one Scheduling, and it's one time_slice,
        /// In other part of the loop, make the process state switch.
        /// </summary>
        void multi_level_feedback_queue()
        {
            double current_time_slice;
            int finished_number = 0;
            while (finished_number< PCB.Count)
            {
                int previous_id = running_id;

                //jishu
                slice_times++;
                current_time_slice = process_schedule(previous_id);

                
                //new code
                if (three_time[running_id].cpu_time1 > 0)
                {
                    double time1 = three_time[running_id].cpu_time1 - current_time_slice;
                    if (time1 > 0)
                    {
                        ready_queue_2.Enqueue(running_id);
                        pcb_contents pc = PCB[running_id];
                        pc.priority = 2;
                        PCB[running_id] = pc;

                        three_times tt1 = three_time[running_id];
                        tt1.cpu_time1 = time1;
                        three_time[running_id] = tt1;

                        //
                        state_switching(running_id, "ready");
                        textBox2.AppendText(System.Environment.NewLine + "====RUNNING TO READY=====" + System.Environment.NewLine);
                        print_state_switching();

                    }

                    // Block
                    else if (time1 <= 0 && three_time[running_id].io_time > 0)
                    {
                        current_time_slice += time1;

                        // update
                        three_times tt1 = three_time[running_id];
                        tt1.cpu_time1 = 0;
                        three_time[running_id] = tt1;

                        if (PCB[running_id].io_type == "Black and white print")
                        {
                            print_queue.Enqueue(running_id);


                        }
                        else
                        {
                            color_printing_queue.Enqueue(running_id);
                        }

                        //
                        state_switching(running_id, "block");
                        textBox2.AppendText(System.Environment.NewLine + "====RUNNING TO BLOCK=====" + System.Environment.NewLine);
                        print_state_switching();

                    }

                    // finshed
                    else
                    {
                        current_time_slice += time1;

                        process_finished();
                        finished_number++;

                        //
                        state_switching(running_id, "finished");
                        textBox2.AppendText(System.Environment.NewLine + "====FINISHED=====" + System.Environment.NewLine);
                        print_state_switching();

                    }
                }

                else
                {
                    double time1 = three_time[running_id].cpu_time2 - current_time_slice;

                    // priority -= 1
                    if (time1 > 0)
                    {
                        ready_queue_2.Enqueue(running_id);
                        pcb_contents pc = PCB[running_id];
                        pc.priority = 2;
                        PCB[running_id] = pc;

                        three_times tt1 = three_time[running_id];
                        tt1.cpu_time1 = time1;
                        three_time[running_id] = tt1;

                        //
                        state_switching(running_id, "ready");
                        textBox2.AppendText(System.Environment.NewLine + "====RUNNING TO READY=====" + System.Environment.NewLine);
                        print_state_switching();

                    }

                    // finshed
                    else
                    {
                        current_time_slice += time1;

                        process_finished();
                        finished_number++;

                        //
                        state_switching(running_id, "finshed");
                        textBox2.AppendText(System.Environment.NewLine + "====FINISHED=====" + System.Environment.NewLine);
                        print_state_switching();

                    }
                }
                

                // Run the IO
                all_process_block_wakeup(current_time_slice);

            }

            // Process revocation
            process_delete();
        }



        /// <summary>
        /// First part of multi_level_feedback_queue
        /// </summary>
        /// <param name="previous_id"></param>
        /// <returns>The size of time slice</returns>
        double process_schedule(int previous_id)
        {
            double current_time_slice;

            if (ready_queue_1.Count > 0)
            {
                running_id = Convert.ToInt32(ready_queue_1.Dequeue());
                current_time_slice = priority_1_slice;

            }

            else if (ready_queue_2.Count > 0)
            {
                running_id = Convert.ToInt32(ready_queue_2.Dequeue());
                current_time_slice = priority_2_slice;

            }

            else
                current_time_slice = 0;

            //
            state_switching(running_id, "running");
            textBox2.AppendText(System.Environment.NewLine + "====PROCESS SCHEDULING=====" + System.Environment.NewLine);
            print_process_scheduling(previous_id, running_id, slice_times);
            textBox2.AppendText(System.Environment.NewLine + "====READY TO RUNNING=====" + System.Environment.NewLine);
            print_state_switching();
            return current_time_slice;
        }

        /// <summary>
        /// Second part of multi_level_feedback_queue
        /// </summary>
        /// <param name="size_slice"></param>
        void all_process_block_wakeup(double size_slice)
        {
            double size_slice2 = size_slice;

            while (size_slice > 0 && print_queue.Count > 0)
            {
                //
                int current_id = Convert.ToInt32(print_queue.Peek());
                size_slice -= three_time[current_id].io_time;

                if (size_slice >= 0)
                {
                    current_id = Convert.ToInt32(print_queue.Dequeue());

                    part_process_block_wakeup(current_id);
                }

                else
                {
                    three_times tt1 = three_time[current_id];
                    tt1.io_time += size_slice;
                    three_time[current_id] = tt1;
                }

            }

            while (size_slice > 0 && color_printing_queue.Count > 0)
            {
                //
                int current_id = Convert.ToInt32(color_printing_queue.Peek());
                size_slice -= three_time[current_id].io_time;

                if (size_slice >= 0)
                {
                    current_id = Convert.ToInt32(color_printing_queue.Dequeue());

                    part_process_block_wakeup(current_id);
                }

                else
                {
                    three_times tt1 = three_time[current_id];
                    tt1.io_time += size_slice;
                    three_time[current_id] = tt1;
                }

            }




        }

        /// <summary>
        /// 2.1
        /// </summary>
        /// <param name="current_id"></param>
        void part_process_block_wakeup(int current_id)
        {
            if (PCB[current_id].priority == 1)
            {
                ready_queue_1.Enqueue(current_id);
            }

            else
            {
                ready_queue_2.Enqueue(current_id);
            }

            textBox2.AppendText(System.Environment.NewLine + "====BLOCK TO READY=====" + System.Environment.NewLine);

            print_state_switching();
            three_times tt1 = three_time[current_id];
            tt1.io_time = 0;
            three_time[current_id] = tt1;
        }

        /// <summary>
        /// Third part of multi_level_feedback_queue
        /// </summary>
        void process_finished()
        {
            pcb_contents pc = PCB[running_id];
            pc.status = "finished";
            PCB[running_id] = pc;
        }


        /// <summary>
        /// Last part of multi_level_feedback_queue
        /// </summary>
        void process_delete()
        {
            PCB.Clear();
            three_time.Clear();
            dataGridView1.RowCount = 0;
            button1.Enabled = true;
            button2.Enabled = true;
            running_id = -1;
            slice_times = 0;
            textBox1.Text = "";

        }

        /**
         *      FOURTH
         *              PRINT
         * */


        /// <summary>
        /// First print
        /// </summary>
        /// <param name="left_process_id">Prevoius process id</param>
        /// <param name="current_process_id"></param>
        /// <param name="slice_times">The times of time_slice</param>
        /// 
        void print_process_scheduling(int left_process_id, int current_process_id, int slice_times)
        {
            pcb_contents current_pcb = PCB[current_process_id];
            pcb_contents left_pcb = PCB[current_process_id];
            textBox2.AppendText(System.Environment.NewLine + System.Environment.NewLine + "The Process ‘" + current_process_id + "’ is running in time - slice " + slice_times + "’”" + System.Environment.NewLine +System.Environment.NewLine);
            textBox2.AppendText("PCB" + System.Environment.NewLine);
            textBox2.AppendText("Current Process ID:" + current_process_id + System.Environment.NewLine + "Status:" + PCB[current_process_id].status + System.Environment.NewLine + "Priority:" + PCB[current_process_id].priority + System.Environment.NewLine + "Runtime:" + current_pcb.runtime + System.Environment.NewLine + "IO time: " + current_pcb.io_time + System.Environment.NewLine + "IO duration:" + current_pcb.io_duration + System.Environment.NewLine + "IO type:" + current_pcb.io_type + System.Environment.NewLine + System.Environment.NewLine);

            if (left_process_id == -1)
                textBox2.AppendText("Previous Process: []");
            else
                textBox2.AppendText("Previous Process ID:" + left_process_id + System.Environment.NewLine + "Status:" + PCB[left_process_id].status + System.Environment.NewLine + "Priority:" + PCB[left_process_id].priority + System.Environment.NewLine + "Runtime:" + left_pcb.runtime + System.Environment.NewLine + "IO time: " + left_pcb.io_time + System.Environment.NewLine + "IO duration:" + left_pcb.io_duration + System.Environment.NewLine + "IO type:" + left_pcb.io_type + System.Environment.NewLine + System.Environment.NewLine);


            //写文件
            write_print_txt();



        }


        /// <summary>
        /// Secind print
        /// </summary>
        void print_state_switching()
        {
            textBox2.AppendText(System.Environment.NewLine + "Priority 1 ready queue: ");
            foreach (var q in ready_queue_1)
                textBox2.AppendText(q + " ");

            //
            textBox2.AppendText(System.Environment.NewLine + "Priority 2 ready queue: ");
            foreach (var q in ready_queue_2)
                textBox2.AppendText(q + " ");
            

            //
            textBox2.AppendText(System.Environment.NewLine + "Black and white block queue: ");
            foreach (var q in print_queue)
                textBox2.AppendText(q + " ");

            //
            textBox2.AppendText(System.Environment.NewLine + "Color printing block queue: ");
            foreach (var q in color_printing_queue)
                textBox2.AppendText(q + " ");

            textBox2.AppendText(System.Environment.NewLine);
            //
            write_print_txt();
        }

        
        /// <summary>
        /// Part of print
        /// </summary>
        void write_print_txt()
        {
            //判断文件是否存在，不存在则创建
            if (!File.Exists(Application.StartupPath + "\\print.txt"))
            {
                FileStream fs1 = new FileStream(Application.StartupPath + "\\print.txt", FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(textBox2.Text);//开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(Application.StartupPath + "\\print.txt", FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(textBox2.Text);//开始写入值
                sr.Close();
                fs.Close();
            }

        }

        /// <summary>
        /// Mark
        /// </summary>
        void print_title()
        {
            string mark = "";
            string wrap = System.Environment.NewLine;
            for (int i = 0; i < 20; i++)
                mark += "*";

            textBox2.Text += wrap + mark + wrap + "         SPMS" + wrap + mark + wrap;

        }
    }


}
