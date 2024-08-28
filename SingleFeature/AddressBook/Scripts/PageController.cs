using System;

namespace VoxelBusters.UseCases
{
    public class PageController
    {
        private int m_maxResultsPerPage;
        private int m_currentOffset;
        private int m_previousOffset;
        private int m_nextOffset;

        public PageController(int maxResultsPerPage = 10, int startingOffset = 0)
        {
            m_maxResultsPerPage = maxResultsPerPage;
            m_currentOffset = m_previousOffset = m_nextOffset = startingOffset;
        }

        public void MoveToNextPage()
        {
            if(m_nextOffset != -1)
            {
                m_previousOffset    = m_currentOffset;
                m_currentOffset     = m_nextOffset;
            }            
        }

        public void MoveToPreviousPage()
        {
            m_currentOffset     = m_previousOffset;
            m_previousOffset    = Math.Max(m_previousOffset - m_maxResultsPerPage, 0);
        }

        public int GetCurrentOffset()
        {
            return m_currentOffset;
        }

        public void UpdateNextOffset(int nextOffset)
        {
            m_nextOffset   = nextOffset;
        }

        public bool HasPreviousPage()
        {
            return m_currentOffset >= m_maxResultsPerPage;
        }

        public bool HasNextPage()
        {
            return m_nextOffset != -1;
        }
    }
}
